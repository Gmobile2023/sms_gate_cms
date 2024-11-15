import { ref } from "vue";
import { useClient, useFormatters } from "@servicestack/vue";
// import { CreateMessage, UpdateMessage } from "dtos.mjs"

export default {
    template: /*html*/`
    <auto-query-grid type="Provider" :visible-from="{ id:'sm', partnerCode:'lg', status:'md', partnerName:'sm', emailAddress:'md', partnerId:'lg', apiKey:'lg', userName:'sm', userName:'md', password:'md', responseDate:'md', ips:'lg' }">
        <template #providerCode-header>Mã nhà cung cấp</template>
        <template #status-header>Trạng thái</template>
        <template #providerName-header>Tên nhà cung cấp</template>
        <template #emailAddress-header>Email</template>
        <template #apiKey-header>apiKey</template>
        <template #phoneNumber-header>Số điện thoại</template>
        <template #userName-header>Tên người dùng</template>
        <template #apiUrl-header>apiUrl</template>
        <template #createdDate-header>Ngày tạo</template>
        <template #modifiedDate-header>Ngày sửa</template>
        <template #createdBy-header>Người tạo</template>
        <template #modifiedBy-header>Người sửa</template>
        
        <template #id="{ id }">
            <span v-html="id"></span>
        </template>

        <template #providerCode="{ providerCode }">
            <span v-html="providerCode"></span>
        </template>

        <template #status="{ status }">
            <span :class="statusClass(status)">{{ statusMap[status] || status }}</span>
        </template>

        <template #providerName="{ providerName }">
            <span v-html="providerName"></span>
        </template>
        <template #EmailAddress="{ emailAddress }">
            <span v-html="emailAddress"></span>
        </template>

        <template #phoneNumber="{ phoneNumber }">
            <span v-html="phoneNumber"></span>
        </template>
        <template #apiKey="{ apiKey }">
            <span v-html="apiKey"></span>
        </template>

        <template #userName="{ userName }">
            <span v-html="userName"></span>
        </template>
        <template #apiUrl="{ apiUrl }">
            <span v-apiUrl="apiUrl"></span>
        </template>
        <template #createdDate="{ createdDate }">
            <span v-html="formatDateTime(createdDate)"></span>
        </template>
         <template #modifiedDate="{ modifiedDate }">
            <span v-html="formatDateTime(modifiedDate)"></span>
        </template>
        <template #createdBy="{ createdBy }">
            <span v-html="createdBy"></span>
        </template>
        <template #modifiedBy="{ modifiedBy }">
            <span v-html="modifiedBy"></span>
        </template>
    </auto-query-grid> 
  `,
    setup() {
        const client = useClient();
        const formatDateTime = (value) => {
            if (!value) return '';

            const date = new Date(value);

            const timeZoneOffset = 7 * 60 * 60000; // Asia/Ho_Chi_Minh có UTC+7
            const localTime = new Date(date.getTime() + timeZoneOffset);

            return new Intl.DateTimeFormat('vi-VN', {dateStyle: 'medium', timeStyle: 'short'}).format(localTime)
        };

        const formatDateTimeLocal = (value) => {
            if (!value) return '';

            const date = new Date(value);

            const localTime = new Date(date.getTime());

            return new Intl.DateTimeFormat('vi-VN', {dateStyle: 'medium', timeStyle: 'short'}).format(localTime)
        };

        const statusMap = {
            "Active": "Hoạt Động",
            "Inactive": "Ngưng hoạt động",
        };

        const statusClass = (status) => {
            return {
                "text-gray-500": status === "Active",
                // "text-blue-500": status === "Sent",
                // "text-green-500": status === "Delivered",
                "text-red-500": status === "Inactive"
            };
        };

        const close = () => importFile.value = null

        return {
            formatDateTime,
            statusMap,
            close,
            formatDateTimeLocal,
            statusClass
        }
    }
}
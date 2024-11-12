import { ref } from "vue";
import { useClient, useFormatters } from "@servicestack/vue";
// import { CreateMessage, UpdateMessage } from "dtos.mjs"

export default {
    template: /*html*/`
    <auto-query-grid type="Partner" :visible-from="{}">
        <template #partnerCode-header>Mã nhà đối tác</template>
        <template #status-header>Trạng thái</template>
        <template #partnerName-header>Tên đối tác</template>
        <template #emailAddress-header>Email</template>
        <template #apiKey-header>apiKey</template>
        <template #userName-header>Tên người dùng</template>
        <template #createdDate-header>Ngày tạo</template>
        <template #modifiedDate-header>Ngày sửa</template>
        <template #createdBy-header>Người tạo</template>
        <template #modifiedBy-header>Người sửa</template>
        
        <template #id="{ id }">
            <span v-text="id"></span>
        </template>
        
        <template #partnerCode="{ partnerCode }">
            <span v-text="partnerCode"></span>
        </template>
        
        <template #status="{ status }">
            <span :class="statusClass(status)">{{ statusMap[status] || status }}</span>
        </template>
        
        <template #partnerName="{ partnerName }">
            <span v-text="partnerName"></span>
        </template>
        
        <template #emailAddress="{ emailAddress }">
            <span v-text="emailAddress"></span>
        </template>
        
        <template #apiKey="{ apiKey }">
            <span v-text="apiKey"></span>
        </template>
        
        <template #userName="{ userName }">
            <span v-text="userName"></span>
        </template>
        
        <template #responseDate="{ responseDate }">
            <span v-text="formatDateTime(responseDate)"></span>
        </template>
        
        <template #ips="{ ips }">
            <span v-text="ips"></span>
        </template>
        
        <template #createdDate="{ createdDate }">
            <span v-text="formatDateTime(createdDate)"></span>
        </template>
        
        <template #modifiedDate="{ modifiedDate }">
            <span v-text="formatDateTime(modifiedDate)"></span>
        </template>
        
        <template #createdBy="{ createdBy }">
            <span v-text="createdBy"></span>
        </template>
        
        <template #modifiedBy="{ modifiedBy }">
            <span v-text="modifiedBy"></span>
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